using AuthService.AppCore.CQRS.Commands;
using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AutoMapper;
using MediatR;

namespace AuthService.AppCore.CQRS.Handlers.CommandHandlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, object>
    {
        private readonly ILogger<ChangePasswordCommand> logger;
        private readonly IAuthRepository _authRepo;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommand> logger, IAuthRepository authRepo, IMapper mapper, IMediator mediator)
        {
            this.logger = logger;
            _authRepo = authRepo;
            this.mapper = mapper;
            this.mediator = mediator;
        }
        public async Task<object> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var passwordresetReq = new ChangePasswordDto()
                {
                    Username = request.Username,
                    OldPassword = request.OldPassword,
                    NewPassword = request.NewPassword,
                    ConfirmPassword = request.ConfirmPassword
                };
                var resp = await _authRepo.ChangePassword(passwordresetReq);
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
