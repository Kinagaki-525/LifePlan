import { FamilyData, IncomeData, SavingsData, ExpensesData, LifeEvent, ChartDataPoint } from '../types';

export function calculateFinancialPlan(
  family: FamilyData,
  income: IncomeData,
  savings: SavingsData,
  expenses: ExpensesData,
  lifeEvents: LifeEvent[]
): ChartDataPoint[] {
  const currentYear = new Date().getFullYear();
  const simulationYears = 60; // 60年間シミュレーション
  const data: ChartDataPoint[] = [];

  let totalAssets = savings.currentSavings * 10000; // 万円を円に変換

  for (let year = 0; year <= simulationYears; year++) {
    const personAge = family.personAge + year;
    const spouseAge = family.spouseAge + year;

    // 年間収入の計算
    let annualIncome = 0;

    // 夫の収入
    if (personAge >= income.husband.workStart && personAge < income.husband.retirementAge) {
      const yearsWorked = personAge - family.personAge;
      annualIncome += income.husband.annualIncome * Math.pow(1 + income.husband.incomeGrowth / 100, yearsWorked) * 10000;
    } else if (personAge >= income.husband.retirementAge) {
      // 退職金（退職した年のみ）
      if (personAge === income.husband.retirementAge) {
        annualIncome += income.husband.retirementBonus * 10000;
      }
      // 年金
      annualIncome += income.husband.pension * 12 * 10000;
    }

    // 妻の収入
    if (spouseAge >= income.wife.workStart && spouseAge < income.wife.retirementAge) {
      const yearsWorked = spouseAge - family.spouseAge;
      annualIncome += income.wife.annualIncome * Math.pow(1 + income.wife.incomeGrowth / 100, yearsWorked) * 10000;
    } else if (spouseAge >= income.wife.retirementAge) {
      // 退職金（退職した年のみ）
      if (spouseAge === income.wife.retirementAge) {
        annualIncome += income.wife.retirementBonus * 10000;
      }
      // 年金
      annualIncome += income.wife.pension * 12 * 10000;
    }

    // その他の収入
    annualIncome += income.otherIncome * 10000;

    // 年間支出の計算
    let annualExpenses = 0;

    // 月々の生活費
    const monthlyLiving = expenses.monthlyLivingCost * Math.pow(1 + expenses.expenseGrowth / 100, year) * 10000;
    annualExpenses += monthlyLiving * 12;

    // 住居費
    if (!expenses.housingEndAge || personAge < expenses.housingEndAge) {
      annualExpenses += expenses.housingCost * 12 * 10000;
    }

    // 教育費（子供の年齢に応じて）
    const children = [
      family.child1Age,
      family.child2Age,
      family.child3Age,
      family.child4Age,
      family.child5Age
    ].filter(age => age !== null) as number[];

    for (const childInitialAge of children) {
      const childAge = childInitialAge + year;
      // 0歳から22歳まで教育費がかかると仮定
      if (childAge >= 0 && childAge <= 22) {
        annualExpenses += (expenses.educationCostPerChild * 10000) / 23; // 23年間で均等割
      }
    }

    // その他の年間支出
    annualExpenses += expenses.carCost * 10000;
    annualExpenses += expenses.insuranceCost * 10000;

    // ライフイベントの支出
    for (const event of lifeEvents) {
      const eventAge = parseInt(event.age);
      const eventCost = parseFloat(event.cost);
      if (!isNaN(eventAge) && !isNaN(eventCost) && personAge === eventAge) {
        annualExpenses += eventCost * 10000;
      }
    }

    // 総資産の更新
    const yearlyChange = annualIncome - annualExpenses;
    totalAssets += yearlyChange;

    // 資産運用による増加（簡易計算）
    if (totalAssets > 0) {
      totalAssets *= (1 + savings.investmentReturn / 100);
    }

    data.push({
      year: currentYear + year,
      age: personAge,
      assets: Math.round(totalAssets),
      income: Math.round(annualIncome),
      expenses: Math.round(annualExpenses)
    });
  }

  return data;
}
