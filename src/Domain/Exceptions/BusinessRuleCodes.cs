namespace Domain.Exceptions;

public static class BusinessRuleCodes
{
    public static class Common
    {
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
        public const string Validation = "VALIDATION_ERROR";
    }

    public static class Buyer
    {
        public const string NotFound = "BUYER_NOT_FOUND";
        public const string EmailAlreadyExists = "BUYER_EMAIL_ALREADY_EXISTS";
        public const string NameRequired = "BUYER_NAME_REQUIRED";
        public const string NameTooShort = "BUYER_NAME_TOO_SHORT";
        public const string NameTooLong = "BUYER_NAME_TOO_LONG";
        public const string EmailRequired = "BUYER_EMAIL_REQUIRED";
        public const string EmailTooLong = "BUYER_EMAIL_TOO_LONG";
        public const string EmailInvalid = "BUYER_EMAIL_INVALID";
    }

    public static class Category
    {
        public const string NotFound = "CATEGORY_NOT_FOUND";
        public const string NameAlreadyExists = "CATEGORY_NAME_ALREADY_EXISTS";
        public const string ParentNotFound = "CATEGORY_PARENT_NOT_FOUND";
        public const string CannotBeOwnParent = "CATEGORY_CANNOT_BE_OWN_PARENT";
        public const string HasChildren = "CATEGORY_HAS_CHILDREN";
        public const string HasProducts = "CATEGORY_HAS_PRODUCTS";
        public const string NameRequired = "CATEGORY_NAME_REQUIRED";
        public const string NameTooShort = "CATEGORY_NAME_TOO_SHORT";
        public const string NameTooLong = "CATEGORY_NAME_TOO_LONG";
        public const string DescriptionTooLong = "CATEGORY_DESCRIPTION_TOO_LONG";
    }

    public static class Product
    {
        public const string NotFound = "PRODUCT_NOT_FOUND";
        public const string NameAlreadyExists = "PRODUCT_NAME_ALREADY_EXISTS";
        public const string CategoriesRequired = "PRODUCT_CATEGORIES_REQUIRED";
        public const string CategoriesNotFound = "PRODUCT_CATEGORIES_NOT_FOUND";
        public const string InUseByOrders = "PRODUCT_IN_USE_BY_ORDERS";
        public const string Inactive = "PRODUCT_INACTIVE";
        public const string NameRequired = "PRODUCT_NAME_REQUIRED";
        public const string NameTooShort = "PRODUCT_NAME_TOO_SHORT";
        public const string NameTooLong = "PRODUCT_NAME_TOO_LONG";
        public const string BrandRequired = "PRODUCT_BRAND_REQUIRED";
        public const string BrandTooLong = "PRODUCT_BRAND_TOO_LONG";
        public const string ColorRequired = "PRODUCT_COLOR_REQUIRED";
        public const string ColorTooLong = "PRODUCT_COLOR_TOO_LONG";
        public const string PriceNegative = "PRODUCT_PRICE_NEGATIVE";
        public const string DescriptionTooLong = "PRODUCT_DESCRIPTION_TOO_LONG";
    }

    public static class Order
    {
        public const string NotFound = "ORDER_NOT_FOUND";
        public const string BuyerRequired = "ORDER_BUYER_REQUIRED";
        public const string BuyerNotFound = "ORDER_BUYER_NOT_FOUND";
        public const string ItemsRequired = "ORDER_ITEMS_REQUIRED";
        public const string DuplicateProduct = "ORDER_DUPLICATE_PRODUCT";
        public const string ProductsNotFound = "ORDER_PRODUCTS_NOT_FOUND";
        public const string CannotUpdate = "ORDER_CANNOT_UPDATE";
        public const string CannotProcess = "ORDER_CANNOT_PROCESS";
        public const string CannotShip = "ORDER_CANNOT_SHIP";
        public const string CannotCancel = "ORDER_CANNOT_CANCEL";
        public const string CannotDelete = "ORDER_CANNOT_DELETE";
        public const string InvalidStatusTransition = "ORDER_INVALID_STATUS_TRANSITION";
        public const string InvalidStatusFilter = "ORDER_INVALID_STATUS_FILTER";
        public const string InvalidDateRange = "ORDER_INVALID_DATE_RANGE";
        public const string ItemProductRequired = "ORDER_ITEM_PRODUCT_REQUIRED";
        public const string ItemQuantityInvalid = "ORDER_ITEM_QUANTITY_INVALID";
        public const string ItemPriceNegative = "ORDER_ITEM_PRICE_NEGATIVE";
    }
}
