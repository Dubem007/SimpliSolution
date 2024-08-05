using AuthService.AppCore.CQRS.Commands;
using AuthService.Domain.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region variable declarations

        private readonly IMediator mediator;

        #endregion

        #region Constructor

        public AuthController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        #endregion
        [HttpPost("signUpUser", Name = nameof(SignUpUser))]
        [ProducesResponseType(typeof(UserCreationResponseDto), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(Summary = "No (1) - This endpoint is used to create new user")]
        public async Task<IActionResult> SignUpUser([FromBody] UserCreationRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var signUpCommandRequest = new SignUpCommand(model);
            var signUpCommandResponse = await this.mediator.Send(signUpCommandRequest);

            return Ok(signUpCommandResponse);
        }

        [HttpPost("loginUser", Name = nameof(LoginUser))]
        [ProducesResponseType(typeof(UserLoginResponse), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(Summary = "No (1) - This endpoint is used to login new user")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var loginCommandRequest = new LoginCommand(model.Username, model.Password);
            var loginCommandResponse = await this.mediator.Send(loginCommandRequest);

            return Ok(loginCommandResponse);
        }

        [HttpPost("unlockuser", Name = nameof(UnlockUser))]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(Summary = "No (1) - This endpoint is used to unlock user")]
        public async Task<IActionResult> UnlockUser([FromBody] UnclockAccountDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var unlockCommandRequest = new UnlockAccountCommand(model);
            var unlockCommandResponse = await this.mediator.Send(unlockCommandRequest);

            return Ok(unlockCommandResponse);
        }

        [HttpPost("changepassword", Name = nameof(ChangePassword))]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(Summary = "No (1) - This endpoint is used to change password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }
            var changeCommandRequest = new ChangePasswordCommand(model);
            var changeCommandResponse = await this.mediator.Send(changeCommandRequest);

            return Ok(changeCommandResponse);
        }
    }
}
