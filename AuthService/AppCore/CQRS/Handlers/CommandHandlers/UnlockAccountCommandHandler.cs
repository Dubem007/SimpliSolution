using AuthService.AppCore.CQRS.Commands;
using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AutoMapper;
using MediatR;

namespace AuthService.AppCore.CQRS.Handlers.CommandHandlers
{
    public class UnlockAccountCommandHandler : IRequestHandler<UnlockAccountCommand, object>
    {
        private readonly ILogger<UnlockAccountCommand> logger;
        private readonly IAuthRepository _authRepo;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public UnlockAccountCommandHandler(ILogger<UnlockAccountCommand> logger, IAuthRepository authRepo, IMapper mapper, IMediator mediator)
        {
            this.logger = logger;
            _authRepo = authRepo;
            this.mapper = mapper;
            this.mediator = mediator;
        }
        public async Task<object> Handle(UnlockAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var unlockaccountReq = new UnclockAccountDto()
                {
                    Username = request.Username,
                    ResetCode = request.ResetCode,
                };
                var resp = await _authRepo.UnlockAccount(unlockaccountReq);
                return resp.Result;

            }
            catch (Exception ex)
            {
                this.logger.LogError(exception: ex, ex.Message);
                return null;
            }
        }
    }
}
