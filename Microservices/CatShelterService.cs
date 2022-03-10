using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microservices.Common.Exceptions;
using Microservices.ExternalServices.Authorization;
using Microservices.ExternalServices.Authorization.Types;
using Microservices.ExternalServices.Billing;
using Microservices.ExternalServices.Billing.Types;
using Microservices.ExternalServices.CatDb;
using Microservices.ExternalServices.CatExchange;
using Microservices.ExternalServices.Database;
using Microservices.Types;
using Polly;

namespace Microservices
{   
    /// <summary>
    /// Представление <see cref="Microservices.Types.Cat"/> для записи в БД
    /// </summary>
    public class CatEntity : IEntityWithId<Guid>
    {
        public Guid Id { get; set; }
        public Cat Cat { get; set; }
    }

    public class CatShelterService : ICatShelterService
    {
        private readonly IDatabase _database;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBillingService _billingService;
        private readonly ICatExchangeService _catExchangeService;
        private readonly ICatInfoService _catInfoService;
        private readonly IAsyncPolicy _policy;

        private const string CatsTableName = "Cats";
        private const int RetryCount = 3;

        public CatShelterService(
            IDatabase database,
            IAuthorizationService authorizationService,
            IBillingService billingService,
            ICatInfoService catInfoService,
            ICatExchangeService catExchangeService)
        {
            _database = database;
            _authorizationService = authorizationService;
            _billingService = billingService;
            _catExchangeService = catExchangeService;
            _catInfoService = catInfoService;
            _policy = Policy
                .Handle<ConnectionException>()
                .RetryAsync(RetryCount, (exception, retry) =>
                {
                    if (retry >= RetryCount)
                    {
                        throw new InternalErrorException(exception);
                    }
                });

        }

        public Task<List<Cat>> GetCatsAsync(string sessionId, int skip, int limit, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddCatToFavouritesAsync(string sessionId, Guid catId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Cat>> GetFavouriteCatsAsync(string sessionId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCatFromFavouritesAsync(string sessionId, Guid catId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> BuyCatAsync(string sessionId, Guid catId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> AddCatAsync(string sessionId, AddCatRequest request, CancellationToken cancellationToken)
        {
            var authorizationResult = await AuthorizeAsync(sessionId, cancellationToken);

            if (!authorizationResult.IsSuccess)
            {
                throw new AuthorizationException();
            }

            var id = Guid.NewGuid();

            var breedInfo = await _policy
                .ExecuteAsync(
                token => _catInfoService.FindByBreedNameAsync(request.Breed, token),
                cancellationToken
                );
            var priceInfo = await _policy
                .ExecuteAsync(
                token => _catExchangeService.GetPriceInfoAsync(breedInfo.BreedId, token),
                cancellationToken
                );

            var cat = new Cat()
            {
                Id = id,
                BreedId = breedInfo.BreedId,
                AddedBy = authorizationResult.UserId,
                Breed = breedInfo.BreedName,
                Name = request.Name,
                CatPhoto = request.Photo,
                BreedPhoto = breedInfo.Photo,
                Price = priceInfo.Prices.Count != 0 ? priceInfo.Prices[^1].Price : 1000,
                Prices = priceInfo.Prices.Select(x => (x.Date, x.Price)).ToList()
            };

            var product = new Product()
            {
                Id = cat.Id,
                BreedId = cat.BreedId
            };
            await _policy
                .ExecuteAsync(
                token => _billingService.AddProductAsync(product, token),
                cancellationToken
                );

            var catEntity = new CatEntity() 
            { 
                Cat = cat, 
                Id = cat.Id 
            };
            _database
                .GetCollection<CatEntity, Guid>(CatsTableName)
                .WriteAsync(catEntity, cancellationToken);

            return cat.Id;

        }

        /// <summary>
        /// Авторизует пользователя
        /// </summary>
        /// <param name="sessionId">ИД сессии</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результат авторизации</returns>
        private async Task<AuthorizationResult> AuthorizeAsync(string sessionId, CancellationToken cancellationToken)
        {
            var authorizationResult =
                await _policy
                .ExecuteAsync(
                    token => _authorizationService.AuthorizeAsync(sessionId, token),
                    cancellationToken
                    );
            return authorizationResult;
        }


    }
}