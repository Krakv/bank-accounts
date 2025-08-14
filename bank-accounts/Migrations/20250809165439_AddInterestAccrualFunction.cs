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
            CREATE OR REPLACE PROCEDURE accrue_interest(p_account_id UUID)
            LANGUAGE plpgsql
            AS $$
            DECLARE
                v_balance      NUMERIC;
                v_interest_rate NUMERIC;
                v_currency     VARCHAR(3);
            BEGIN
                SELECT ""Balance"", ""InterestRate"", ""Currency""
                INTO v_balance, v_interest_rate, v_currency
                FROM ""Accounts""
                WHERE ""Id"" = p_account_id
                  AND ""InterestRate"" IS NOT NULL
                  AND (""ClosingDate"" IS NULL OR ""ClosingDate"" > NOW())
                FOR UPDATE;

                IF NOT FOUND THEN
                    RAISE NOTICE 'Account % not eligible for interest accrual', p_account_id;
                    RETURN;
                END IF;

                DECLARE
                    v_interest_amount NUMERIC := ROUND(v_balance * v_interest_rate / 100 / 12, 2);
                BEGIN
                    UPDATE ""Accounts""
                    SET ""Balance"" = ""Balance"" + v_interest_amount
                    WHERE ""Id"" = p_account_id;

                    INSERT INTO ""Transactions"" (
                        ""Id"", ""AccountId"", ""Currency"", ""Date"", ""Description"", ""Type"", ""Value""
                    ) VALUES (
                        gen_random_uuid(),
                        p_account_id,
                        v_currency,
                        NOW(),
                        'Accrued interest',
                        'Credit',
                        v_interest_amount
                    );
                END;
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
