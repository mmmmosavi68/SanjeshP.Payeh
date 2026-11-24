namespace SanjeshP.Payeh.Common;

/// <summary>پیام فارسی همراه با ErrorCode پایدار برای ترجمه در Frontend.</summary>
public sealed record ApiMessage(string Code, string Text)
{
    public override string ToString() => Text;

    public static implicit operator string(ApiMessage message) => message.Text;
}
