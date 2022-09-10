namespace CSBarcode.QR.Exceptions;

public class MessageTooLongException : Exception
{

    public MessageTooLongException()
    {
    }

    public MessageTooLongException(string message) : base(message)
    {
    }

    public MessageTooLongException(string message, Exception inner) : base(message, inner)
    {
    }

}