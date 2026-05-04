using LifePlan.Domain.ReferenceData;
using LifePlan.Domain.Rules;
using LifePlan.Domain.Entities;
using LifePlan.Domain.Logic;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Mappers
{
    public static class LifePlanPageMapper
    {
        public static List<ChildInputViewModel> CreateChildInputs()
        {
            return CreateChildLabels()
                .Select(label => new ChildInputViewModel { Label = label })
                .ToList();
        }

        public static List<ChildEducationInputViewModel> CreateEducationPlans()
        {
            return CreateChildLabels()
                .Select(label => new ChildEducationInputViewModel { Label = label })
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> CreateChildAgeOptions()
        {
            var options = new List<SelectOptionViewModel>
            {
                new(string.Empty, "-（なし）")
            };

            for (var age = AgeRules.MinChildAge; age <= AgeRules.MaxChildAge; age++)
            {
                options.Add(new SelectOptionViewModel(age.ToString(), age.ToString()));
            }

            return options;
        }

        public static IReadOnlyList<SelectOptionViewModel> ToEducationOptions(IReadOnlyList<EducationCostEntry> entries)
        {
            return entries
                .Select(entry => new SelectOptionViewModel(entry.Value, $"{entry.Stage}：{entry.Type}"))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToEducationOptionsByStage(
            IReadOnlyList<EducationCostEntry> entries,
            string stage)
        {
            return entries
                .Where(entry => entry.Stage == stage)
                .Select(entry => new SelectOptionViewModel(entry.Value, entry.Type))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToOccupationOptions(IReadOnlyList<OccupationEntry> occupations)
        {
            return occupations
                .Select(occupation => new SelectOptionViewModel(occupation.Value, occupation.DisplayName))
                .ToList();
        }

        public static LifePlanData ToLifePlanData(LifePlanViewModel model)
        {
            return new LifePlanData
            {
                Family = new FamilyData
                {
                    HusbandAge = model.Family.HusbandAge.GetValueOrDefault(),
                    WifeAge = model.Family.WifeAge.GetValueOrDefault(),
                    Children = model.Family.Children
                        .Select(child => new ChildData { Age = child.Age })
                        .ToList()
                },
                LifeEvents = new LifeEventData
                {
                    Marriage = ToMarriageEventData(model.LifeEvents.Marriage),
                    Housing = ToHousingEventData(model.LifeEvents.Housing),
                    Car = ToCarEventData(model.LifeEvents.Car),
                    EducationPlans = model.LifeEvents.EducationPlans
                        .Select(ToChildEducationData)
                        .ToList(),
                    TravelOther = ToTravelOtherEventData(model.LifeEvents.TravelOther)
                },
                Assets = new AssetData
                {
                    CurrentFinancialAssetsYen = ToYen(model.Savings.CurrentFinancialAssetsManYen),
                    ExpectedAnnualReturnRatePercent = model.Savings.ExpectedAnnualReturnRatePercent
                },
                IncomeExpense = new IncomeExpenseData
                {
                    HusbandIncome = ToPersonIncomeData(model.IncomeExpense.HusbandIncome),
                    WifeIncome = ToPersonIncomeData(model.IncomeExpense.WifeIncome),
                    Expenses = ToExpenseData(model.IncomeExpense.Expenses)
                }
            };
        }

        public static LifePlanResultViewModel ToResultViewModel(LifePlanCalculationResult result)
        {
            var rows = result.AnnualRows;
            var cashFlowRows = new List<CashFlowTableRowViewModel>
            {
                CreateRow("家族構成", "family", "夫", rows.Select(row => ToAgeText(row.HusbandAge))),
                CreateRow("家族構成", "family", "妻", rows.Select(row => ToAgeText(row.WifeAge)))
            };

            AddChildRows(cashFlowRows, rows);
            AddIncomeRows(cashFlowRows, rows);
            AddExpenseRows(cashFlowRows, rows);
            AddSavingsRows(cashFlowRows, rows);

            return new LifePlanResultViewModel
            {
                YearHeaders = rows.Select(row => row.Year.ToString()).ToList(),
                CashFlowRows = cashFlowRows,
                ChartPoints = rows.Select(ToChartPointViewModel).ToList(),
                Assumptions = LifePlanAssumptionMapper.CreateAssumptions()
            };
        }

        private static IReadOnlyList<string> CreateChildLabels()
        {
            return
            [
                "第1子",
                "第2子",
                "第3子",
                "第4子"
            ];
        }

        private static void AddChildRows(
            List<CashFlowTableRowViewModel> cashFlowRows,
            IReadOnlyList<AnnualCashFlowRow> rows)
        {
            var maxChildCount = rows
                .Select(row => row.ChildAges.Count)
                .DefaultIfEmpty()
                .Max();

            for (var index = 0; index < maxChildCount; index++)
            {
                var values = rows
                    .Select(row => index < row.ChildAges.Count ? ToAgeText(row.ChildAges[index]) : "-")
                    .ToList();

                if (values.All(value => value == "-"))
                {
                    continue;
                }

                cashFlowRows.Add(new CashFlowTableRowViewModel
                {
                    Category = "家族構成",
                    Label = $"第{index + 1}子",
                    CategoryKey = "family",
                    Values = values
                });
            }
        }

        private static void AddIncomeRows(
            List<CashFlowTableRowViewModel> cashFlowRows,
            IReadOnlyList<AnnualCashFlowRow> rows)
        {
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "夫 給与", rows.Select(row => row.HusbandIncome.SalaryYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "夫 退職金", rows.Select(row => row.HusbandIncome.RetirementAllowanceYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "夫 年金", rows.Select(row => row.HusbandIncome.PensionYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "妻 給与", rows.Select(row => row.WifeIncome.SalaryYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "妻 退職金", rows.Select(row => row.WifeIncome.RetirementAllowanceYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "妻 年金", rows.Select(row => row.WifeIncome.PensionYen)));
            cashFlowRows.Add(CreateMoneyRow("収入", "income", "収入合計", rows.Select(row => row.TotalIncomeYen)));
        }

        private static void AddExpenseRows(
            List<CashFlowTableRowViewModel> cashFlowRows,
            IReadOnlyList<AnnualCashFlowRow> rows)
        {
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "基本生活費", rows.Select(row => row.Expenses.BasicLivingCostYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "家賃", rows.Select(row => row.Expenses.RentYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "その他支出", rows.Select(row => row.Expenses.OtherAnnualCostYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "結婚", rows.Select(row => row.Expenses.MarriageYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "住宅頭金", rows.Select(row => row.Expenses.HousingDownPaymentYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "住宅ローン返済", rows.Select(row => row.Expenses.HousingLoanRepaymentYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "自動車", rows.Select(row => row.Expenses.CarYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "教育費", rows.Select(row => row.Expenses.EducationYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "旅行・その他", rows.Select(row => row.Expenses.TravelOtherYen)));
            cashFlowRows.Add(CreateMoneyRow("支出", "expense", "支出合計", rows.Select(row => row.TotalExpenseYen)));
        }

        private static void AddSavingsRows(
            List<CashFlowTableRowViewModel> cashFlowRows,
            IReadOnlyList<AnnualCashFlowRow> rows)
        {
            cashFlowRows.Add(CreateMoneyRow("貯蓄", "savings", "開始時点金融資産", rows.Select(row => row.StartingAssetsYen)));
            cashFlowRows.Add(CreateMoneyRow("貯蓄", "savings", "収入－支出", rows.Select(row => row.AnnualBalanceYen)));
            cashFlowRows.Add(CreateMoneyRow("貯蓄", "savings", "貯蓄合計（0%運用）", rows.Select(row => row.SavingsBalanceWithoutReturnYen)));
            cashFlowRows.Add(CreateMoneyRow("貯蓄", "savings", "貯蓄合計（想定年利運用）", rows.Select(row => row.SavingsBalanceWithReturnYen)));
        }

        private static CashFlowTableRowViewModel CreateMoneyRow(
            string category,
            string categoryKey,
            string label,
            IEnumerable<long> values)
        {
            return CreateRow(category, categoryKey, label, values.Select(ToManYenText));
        }

        private static CashFlowTableRowViewModel CreateRow(
            string category,
            string categoryKey,
            string label,
            IEnumerable<string> values)
        {
            return new CashFlowTableRowViewModel
            {
                Category = category,
                Label = label,
                CategoryKey = categoryKey,
                Values = values.ToList()
            };
        }

        private static string ToAgeText(int? age)
        {
            return age.HasValue ? age.Value.ToString() : "-";
        }

        private static string ToManYenText(long yen)
        {
            return ToManYen(yen).ToString("0.0");
        }

        private static decimal ToManYen(long yen)
        {
            return yen / 10000m;
        }

        private static LifePlanChartPointViewModel ToChartPointViewModel(AnnualCashFlowRow row)
        {
            return new LifePlanChartPointViewModel
            {
                Year = row.Year,
                TotalIncomeManYen = ToManYen(row.TotalIncomeYen),
                TotalExpenseManYen = ToManYen(row.TotalExpenseYen),
                SavingsBalanceWithoutReturnManYen = ToManYen(row.SavingsBalanceWithoutReturnYen),
                SavingsBalanceWithReturnManYen = ToManYen(row.SavingsBalanceWithReturnYen)
            };
        }

        private static PersonIncomeData ToPersonIncomeData(PersonIncomeInputViewModel input)
        {
            return new PersonIncomeData
            {
                OccupationValue = input.OccupationValue,
                AnnualIncomeYen = ToYen(input.AnnualIncomeManYen),
                AnnualIncomeChangeRatePercent = input.AnnualIncomeChangeRatePercent,
                WorkStartAge = input.WorkStartAge,
                WorkEndAge = input.WorkEndAge,
                RetirementAllowanceYen = ToYen(input.RetirementAllowanceManYen),
                AnnualPensionYen = ToYen(input.AnnualPensionManYen),
                PensionStartAge = input.PensionStartAge
            };
        }

        private static MarriageEventData ToMarriageEventData(MarriageEventInputViewModel input)
        {
            return new MarriageEventData
            {
                CostYen = ToYen(input.CostManYen),
                HusbandAge = input.HusbandAge
            };
        }

        private static HousingEventData ToHousingEventData(HousingEventInputViewModel input)
        {
            return new HousingEventData
            {
                DownPaymentYen = ToYen(input.DownPaymentManYen),
                BorrowingAmountYen = ToYen(input.BorrowingAmountManYen),
                PurchaseHusbandAge = input.PurchaseHusbandAge,
                LoanYears = input.LoanYears,
                InterestRatePercent = input.InterestRatePercent
            };
        }

        private static CarEventData ToCarEventData(CarEventInputViewModel input)
        {
            return new CarEventData
            {
                PurchaseAmountYen = ToYen(input.PurchaseAmountManYen),
                FirstPurchaseHusbandAge = input.FirstPurchaseHusbandAge,
                ReplacementIntervalYears = input.ReplacementIntervalYears
            };
        }

        private static ChildEducationData ToChildEducationData(ChildEducationInputViewModel input)
        {
            return new ChildEducationData
            {
                NurseryOptionValue = input.NurseryOptionValue,
                KindergartenOptionValue = input.KindergartenOptionValue,
                ElementarySchoolOptionValue = input.ElementarySchoolOptionValue,
                JuniorHighSchoolOptionValue = input.JuniorHighSchoolOptionValue,
                HighSchoolOptionValue = input.HighSchoolOptionValue,
                UniversityOptionValue = input.UniversityOptionValue,
                GraduateSchoolOptionValue = input.GraduateSchoolOptionValue
            };
        }

        private static TravelOtherEventData ToTravelOtherEventData(TravelOtherEventInputViewModel input)
        {
            return new TravelOtherEventData
            {
                AnnualCostYen = ToYen(input.AnnualCostManYen),
                StartHusbandAge = input.StartHusbandAge,
                EndHusbandAge = input.EndHusbandAge
            };
        }

        private static ExpenseData ToExpenseData(ExpenseInputViewModel input)
        {
            return new ExpenseData
            {
                MonthlyBasicLivingCostYen = ToYen(input.MonthlyBasicLivingCostManYen),
                InflationRatePercent = input.InflationRatePercent,
                MonthlyRentYen = ToYen(input.MonthlyRentManYen),
                OtherAnnualCostYen = ToYen(input.OtherAnnualCostManYen)
            };
        }

        private static long? ToYen(decimal? manYen)
        {
            return manYen.HasValue
                ? decimal.ToInt64(decimal.Round(manYen.Value * 10000m, 0, MidpointRounding.AwayFromZero))
                : null;
        }
    }
}
