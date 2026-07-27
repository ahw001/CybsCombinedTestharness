using System.Collections.Generic;

namespace CybsClient.Model.Cybersource.Boarding;

public class BoardingDashboardDto
{
    public List<BoardingDashboardOrgItem> Organizations { get; set; } = new();
    public BoardingErrorDto? Error { get; set; }
}

public class BoardingDashboardOrgItem
{
    public int BoardingOrganizationId { get; set; }
    public string? OrganizationId { get; set; }
    public string? BusinessName { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? BoardingFlow { get; set; }
    public List<BoardingDashboardMerchantItem> TransactingMerchants { get; set; } = new();
}

public class BoardingDashboardMerchantItem
{
    public int BoardingTransactingMerchantId { get; set; }
    public string? TransactingOrganizationId { get; set; }
    public string? BusinessName { get; set; }
    public string? Status { get; set; }
    public string? BoardingProcessor { get; set; }
    public string? CybersourceBoardingStatus { get; set; }
    public List<BoardingDashboardCardProductItem> CardProductSubscriptions { get; set; } = new();
    public List<BoardingDashboardSupplementalProductItem> SupplementalProducts { get; set; } = new();
}

public class BoardingDashboardCardProductItem
{
    public int BoardingProductSubscriptionId { get; set; }
    public int BoardingTransactingMerchantSubscriptionId { get; set; }
    public string? ProductName { get; set; }
    public string? EnablementStatus { get; set; }
    public string? CybersourceBoardingStatus { get; set; }
    public bool? CardPresentEnabled { get; set; }
    public bool? CardNotPresentEnabled { get; set; }
    public bool IncludeInBoarding { get; set; } = true;
    public List<BoardingDashboardProcessorItem> Processors { get; set; } = new();
}

public class BoardingDashboardProcessorItem
{
    public int BoardingProcessorConfigId { get; set; }
    public string? ProcessorName { get; set; }
}

public class BoardingDashboardSupplementalProductItem
{
    public int BoardingTransactingMerchantProductSubscriptionId { get; set; }
    public string? ProductType { get; set; }
    public int ProductSubscriptionId { get; set; }
    public bool? Enabled { get; set; }
    public string? EnablementStatus { get; set; }
    public bool IncludeInBoarding { get; set; } = true;
    public string? CybersourceBoardingStatus { get; set; }
    public string? NtCybersourceBoardingStatus { get; set; }
}
