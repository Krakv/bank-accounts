﻿using bank_accounts.Features.Transactions.Dto;
using MediatR;

namespace bank_accounts.Features.Transactions.GetTransaction;

public class GetTransactionQuery(Guid id) : IRequest<TransactionDto?>
{
    public Guid Id { get; set; } = id;
}