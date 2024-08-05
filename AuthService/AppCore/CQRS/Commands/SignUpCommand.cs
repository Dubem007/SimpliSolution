using AuthService.Domain.Dtos;
using MediatR;

namespace AuthService.AppCore.CQRS.Commands
{
    public class SignUpCommand : IRequest<UserCreationResponseDto>
    {
        public SignUpCommand(UserCreationRequestDto entity)
        {

            this.Entity = entity;
        }

        public UserCreationRequestDto Entity { get; private set; }
    }
}
