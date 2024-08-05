using AuthService.AppCore.CQRS.Commands;
using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AutoMapper;
using MediatR;
using OnaxTools.Http;

namespace AuthService.AppCore.CQRS.Handlers.CommandHandlers
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, UserCreationResponseDto>
    {
        #region Variable Declarations
        private readonly ILogger<SignUpCommandHandler> logger;
        private readonly IAuthRepository _authRepo;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public SignUpCommandHandler(ILogger<SignUpCommandHandler> logger, IAuthRepository authRepo, IMapper mapper, IMediator mediator)
        {
            this.logger = logger;
            _authRepo = authRepo;
            this.mapper = mapper;
            this.mediator = mediator;
        }
        #endregion
        public async Task<UserCreationResponseDto> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
			try
			{
                var resp = await _authRepo.CreateUser(request.Entity);
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
