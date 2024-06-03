class ChatNicknameNotSetException : Exception
{
    public ChatNicknameNotSetException() : base("O usuário não definiu um nickname.") { }
}

class ChatNotAuthenticatedException : Exception
{
    public ChatNotAuthenticatedException() : base("O usuário não está autenticado.") { }
}

class ChatAlreadyAuthenticatedException : Exception
{
    public ChatAlreadyAuthenticatedException() : base("O usuário já está autenticado.") { }
}