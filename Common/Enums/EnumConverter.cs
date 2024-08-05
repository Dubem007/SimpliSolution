namespace Common.Enums
{
    public static class EnumConverter
    {
        public static string GetNameFromClientProvidedNameEnum(ClientProvidedNameEnum clientProvidedNameEnum) => clientProvidedNameEnum.ToString();

        public static string GetNameFromExchangeNameEnum(ExchangeNameEnum exchangeNameEnum) => exchangeNameEnum.ToString();
    }
}
