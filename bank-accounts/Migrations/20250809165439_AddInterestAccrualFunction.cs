using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bank_accounts.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestAccrualFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION accrue_interest(p_account_id UUID)
            RETURNS TABLE (
                account_id UUID,
                period_from TIMESTAMP,
                period_to TIMESTAMP,
                amount NUMERIC
            ) LANGUAGE plpgsql AS $$
            DECLARE
                v_account RECORD;
                v_period_from TIMESTAMP;
                v_period_to TIMESTAMP := NOW();
                v_interest NUMERIC;
            BEGIN
                SELECT * INTO v_account
                FROM ""Accounts""
                WHERE ""Id"" = p_account_id
                  AND ""InterestRate"" IS NOT NULL
                  AND (""ClosingDate"" IS NULL OR ""ClosingDate"" > NOW())
                FOR UPDATE;

                IF NOT FOUND OR v_account.""Balance"" <= 0 THEN
                    RETURN;
                END IF;

                SELECT COALESCE(MAX(""Date"") + interval '1 second', NOW()) INTO v_period_from
                FROM ""Transactions""
                WHERE ""AccountId"" = p_account_id
                  AND ""Description"" = 'Начисление процентов по вкладу';

                IF v_period_from > v_period_to THEN
                    RETURN;
                END IF;

                v_interest := ROUND(v_account.""Balance"" * v_account.""InterestRate"" / 100 / 365, 2);

                IF v_interest < 0.01 THEN
                    RETURN;
                END IF;

                UPDATE ""Accounts""
                SET ""Balance"" = ""Balance"" + v_interest
                WHERE ""Id"" = p_account_id;

                INSERT INTO ""Transactions""
                    (""Id"", ""AccountId"", ""Currency"", ""Date"", ""Description"", ""Type"", ""Value"")
                VALUES (
                    gen_random_uuid(),
                    p_account_id,
                    v_account.""Currency"",
                    v_period_to,
                    'Начисление процентов по вкладу',
                    'Credit',
                    v_interest
                );

                RETURN QUERY
                SELECT
                    p_account_id,
                    v_period_from,
                    v_period_to,
                    v_interest;
            END;
            $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS accrue_interest(UUID)");
        }
    }
}
