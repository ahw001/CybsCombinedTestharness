using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.Cybersource.Models.Mappers
{
    public static class ApplePayTransactionMapper
    {
        public static ApplePayTransactionDto? Map(ApplePayTransaction applePayTransaction)
        {
            return applePayTransaction != null ? new ApplePayTransactionDto
            {
                ApplePayTransactionsId = applePayTransaction.ApplePayTransactionsId,
                Id = applePayTransaction.Id,
                OrderId = applePayTransaction.OrderId,
                ClientReferenceCode = applePayTransaction.ClientReferenceCode,
                Status = applePayTransaction.Status,
                SubmitTimeUtc = applePayTransaction.SubmitTimeUtc,
                AuthorizedAmount = applePayTransaction.AuthorizedAmount,
                Currency = applePayTransaction.Currency,
                CardNetwork = applePayTransaction.CardNetwork,
                MaskedPan = applePayTransaction.MaskedPan,
                PaymentSolution = applePayTransaction.PaymentSolution,
                TransactionType = applePayTransaction.TransactionType,
                DecryptedTokenJson = applePayTransaction.DecryptedTokenJson,
                RequestTransactionJson = applePayTransaction.RequestTransactionJson,
                ResponseTransactionJson = applePayTransaction.ResponseTransactionJson,
            } : null;
        }

        public static ApplePayTransaction? Map(ApplePayTransactionDto applePayTransactionDto)
        {
            return applePayTransactionDto != null ? new ApplePayTransaction
            {
                ApplePayTransactionsId = applePayTransactionDto.ApplePayTransactionsId,
                Id = applePayTransactionDto.Id!,
                OrderId = applePayTransactionDto.OrderId,
                ClientReferenceCode = applePayTransactionDto.ClientReferenceCode,
                Status = applePayTransactionDto.Status,
                SubmitTimeUtc = applePayTransactionDto.SubmitTimeUtc,
                AuthorizedAmount = applePayTransactionDto.AuthorizedAmount,
                Currency = applePayTransactionDto.Currency,
                CardNetwork = applePayTransactionDto.CardNetwork,
                MaskedPan = applePayTransactionDto.MaskedPan,
                PaymentSolution = applePayTransactionDto.PaymentSolution,
                TransactionType = applePayTransactionDto.TransactionType,
                DecryptedTokenJson = applePayTransactionDto.DecryptedTokenJson,
                RequestTransactionJson = applePayTransactionDto.RequestTransactionJson,
                ResponseTransactionJson = applePayTransactionDto.ResponseTransactionJson,
            } : null;
        }

        public static List<ApplePayTransaction> Map(List<ApplePayTransactionDto> applePayTransactionDtos)
        {
            return applePayTransactionDtos.Select(Map).ToList()!;
        }

        public static List<ApplePayTransactionDto> Map(List<ApplePayTransaction> applePayTransactions)
        {
            return applePayTransactions.Select(Map).ToList()!;
        }
    }
}
