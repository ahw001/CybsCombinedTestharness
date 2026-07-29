namespace CybsClient.Model
{
    public enum CcTransactionTypes
    {
        AUTH,
        SALE,
        CAPTURE,
        REVERSAL,
        VOID_SALE,
        VOID_CAPTURE,
        VOID_CREDIT,
        VOID_REFUND,
        REFUND_SALE,
        REFUND_CAPTURE,
        CREDIT,
        TOKEN_CREATE,
        TOKENIZE_CARD,
        INST_ID_CREATE,
        INST_ID_DELETE,
        INST_ID_UPDATE,
        INST_ID_RETRIEVE,
        PAY_ID_CREATE,
        PAY_ID_DELETE,
        PAY_ID_UPDATE,
        PAY_ID_RETRIEVE,
        CUST_ID_CREATE,
        CUST_ID_DELETE,
        CUST_ID_UPDATE,
        CUST_ID_RETRIEVE,
        TOKEN_DECRYPT,
        SHIPPING_ID_CREATE,
        SHIPPING_ID_DELETE,
        SHIPPING_ID_UPDATE,
        SHIPPING_ID_RETRIEVE,
        UNIFIED_CHECKOUT,
        UNIFIED_CHECKOUT_V1,
        UNIFIED_CHECKOUT_PAYMENT,
        FLEX_CHECKOUT,
        FLEX_CHECKOUT_PAYMENT,
        FLEX_PA_SETUP,
        TRANS_TOKEN_INFORMATION,
        INVOICE_CREATE,
        PA_SETUP,
        PA_ENROLL,
        PA_VALIDATE,
        MERCHANT_BOARDING_CREATE,
        TRANSACTION_BOARDING_CREATE,
        CLOUD_POS_BEARER_CREATE,
        CLOUD_POS_BEARER_SALE,
        CLOUD_POS_BEARER_PRE_AUTH,
        CLOUD_POS_BEARER_RETURN,
        CLOUD_POS_BEARER_STATUS_CHECK,
        CLOUD_POS_BEARER_STANDALONE_RETURN,
        CLOUD_POS_CANCEL,
        CLOUD_POS_TOKEN_RETURN,
        CLOUD_POS_CAPTURE,
        SEMI_POS_SETUP,
        SEMI_POS_SALE,
        SEMI_POS_PRE_AUTH,
        SEMI_POS_STATUS_CHECK,
        SEMI_POS_STANDALONE_RETURN,
        SEMI_POS_CANCEL,
        SEMI_POS_TOKEN_RETURN,
        SEMI_POS_CAPTURE,
        SIMPLE_STRING,
        STANDALONE_AFT_TRANSACTION,
        TOKEN_AFT_TRANSACTION,
        FLEX_AFT_TRANSACTION,
        FLEX_AFT_CHECK_ENROLL_AUTH,
        FLEX_AFT_VALIDATE_AUTH,
        SAMPLE_CARD_LIST,
        GET_CATEGORY_LIST,
        RANDOM_CUSTOMER,
        SAMPLE_INVOICE,
        SAMPLE_PA_CARDS,
        SAMPLE_CART,
        SINGLE_SAMPLE_MERCHANT,
        SAMPLE_AFT,
        SESSION_STATE_STORE,
        SESSION_STATE_RETRIEVE,
        PARTIAL_AUTH_BALANCE,
        APPLE_PAY_AUTH,

        // ── Boarding: Processors ─────────────────────────────────────────────
        GetAllProcessors,
        GetProcessorById,
        CreateProcessor,
        UpdateProcessor,
        DeleteProcessor,

        // ── Boarding: Product Subscriptions (master boarding endpoint) ────────
        GetAllProductSubscriptions,
        GetProductSubscriptionById,
        SaveProductSubscription,
        DeleteProductSubscription,

        // ── Boarding: Organizations ───────────────────────────────────────────
        GetAllOrganizations,
        GetOrganizationById,
        CreateOrganization,
        UpdateOrganization,
        DeleteOrganization,

        // ── Boarding: Active Product Sets ─────────────────────────────────────
        GetAllActiveProductSets,
        GetActiveProductSetById,
        SaveActiveProductSet,
        DeleteActiveProductSet,

        // ── Boarding: Products Catalog ────────────────────────────────────────
        GetAllBoardingProducts,
        GetBoardingProductById,
        SaveBoardingProduct,
        DeleteBoardingProduct,

        // ── Boarding: Features Catalog ────────────────────────────────────────
        GetAllFeatures,
        GetFeatureById,
        CreateFeature,
        UpdateFeature,
        DeleteFeature,

        // ── Boarding: Boarding Requests ───────────────────────────────────────
        GetAllBoardingRequests,
        GetBoardingRequestById,
        CreateBoardingRequest,
        UpdateBoardingRequest,
        DeleteBoardingRequest,

        // ── Boarding: Dashboard ───────────────────────────────────────────────
        GetBoardingDashboard,

        // ── Store Unified Checkout: manual transient-token pattern ────────────
        UNIFIED_CHECKOUT_V0_CONTEXT,     // POST /api/tokens/v0sessioncontext (config-driven v0 ctx)
        UNIFIED_CHECKOUT_TOKEN_PAYMENT,  // POST /api/unified/v1tokenpayment (follow-on /pts/v2/payments)

    }
    public enum TransactionStateValues
    {
        NotStarted,
        InProcess,
        Complete,
        Stale,
        Error,
        Unknown
    }

    public enum TransactionResponseValues
    {
        AUTHORIZED,
        PARTIAL_AUTHORIZED,
        AUTHORIZED_PENDING_REVIEW,
        AUTHORIZED_RISK_DECLINED,
        PENDING_AUTHENTICATION,
        PENDING_REVIEW,
        DECLINED,
        INVALID_REQUEST
    }
}
