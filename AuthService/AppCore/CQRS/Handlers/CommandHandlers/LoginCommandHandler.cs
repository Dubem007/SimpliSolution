using AuthService.AppCore.CQRS.Commands;
using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AutoMapper;
using MediatR;

namespace AuthService.AppCore.CQRS.Handlers.CommandHandlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, UserLoginResponse>
    {
        private readonly ILogger<LoginCommandHandler> logger;
        private readonly IAuthRepository _authRepo;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public LoginCommandHandler(ILogger<LoginCommandHandler> logger, IAuthRepository authRepo, IMapper mapper, IMediator mediator)
        {
            this.logger = logger;
            _authRepo = authRepo;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        public async Task<UserLoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var loginReq = new UserLoginDto()
                {
                    Username = request.UserName,
                    Password = request.password,
                };
                var resp = await _authRepo.Login(loginReq);
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
