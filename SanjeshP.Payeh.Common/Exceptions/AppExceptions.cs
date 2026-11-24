using System.Net;

namespace SanjeshP.Payeh.Common.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException() : this(ApiMessages.Common.Unauthorized) { }
        public UnauthorizedException(ApiMessage message) : base(ApiResultStatusCode.UnAuthorized, message.Text, HttpStatusCode.Unauthorized) { ErrorCode = message.Code; }
        public UnauthorizedException(string message) : base(ApiResultStatusCode.UnAuthorized, message, HttpStatusCode.Unauthorized) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException() : this(ApiMessages.Common.Forbidden) { }
        public ForbiddenException(ApiMessage message) : base(ApiResultStatusCode.Forbidden, message.Text, HttpStatusCode.Forbidden) { ErrorCode = message.Code; }
        public ForbiddenException(string message) : base(ApiResultStatusCode.Forbidden, message, HttpStatusCode.Forbidden) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException() : this(ApiMessages.Common.Conflict) { }
        public ConflictException(ApiMessage message) : base(ApiResultStatusCode.Conflict, message.Text, HttpStatusCode.Conflict) { ErrorCode = message.Code; }
        public ConflictException(string message) : base(ApiResultStatusCode.Conflict, message, HttpStatusCode.Conflict) { }
    }

    public class TooManyRequestsException : AppException
    {
        public TooManyRequestsException() : this(ApiMessages.Common.TooManyRequests) { }
        public TooManyRequestsException(ApiMessage message) : base(ApiResultStatusCode.TooManyRequests, message.Text, HttpStatusCode.TooManyRequests) { ErrorCode = message.Code; }
        public TooManyRequestsException(string message) : base(ApiResultStatusCode.TooManyRequests, message, HttpStatusCode.TooManyRequests) { }
    }

    public class CaptchaInvalidException : AppException
    {
        public CaptchaInvalidException() : this(ApiMessages.Common.CaptchaInvalid) { }
        public CaptchaInvalidException(ApiMessage message) : base(ApiResultStatusCode.CaptchaInvalid, message.Text, HttpStatusCode.BadRequest) { ErrorCode = message.Code; }
        public CaptchaInvalidException(string message) : base(ApiResultStatusCode.CaptchaInvalid, message, HttpStatusCode.BadRequest) { }
    }

    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException() : this(ApiMessages.Common.InvalidCredentials) { }
        public InvalidCredentialsException(ApiMessage message) : base(ApiResultStatusCode.InvalidCredentials, message.Text, HttpStatusCode.BadRequest) { ErrorCode = message.Code; }
        public InvalidCredentialsException(string message) : base(ApiResultStatusCode.InvalidCredentials, message, HttpStatusCode.BadRequest) { }
    }

    public class InvalidOtpException : AppException
    {
        public InvalidOtpException(ApiMessage message) : base(ApiResultStatusCode.InvalidOtp, message.Text, HttpStatusCode.BadRequest) { ErrorCode = message.Code; }
        public InvalidOtpException(string message) : base(ApiResultStatusCode.InvalidOtp, message, HttpStatusCode.BadRequest) { }
    }
}
