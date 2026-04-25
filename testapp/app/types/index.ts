export interface FamilyData {
  personAge: number;
  spouseAge: number;
  child1Age: number | null;
  child2Age: number | null;
  child3Age: number | null;
  child4Age: number | null;
  child5Age: number | null;
}

export interface IncomeData {
  husband: {
    annualIncome: number;
    incomeGrowth: number;
    workStart: number;
    workEnd: number;
    retirementAge: number;
    retirementBonus: number;
    pension: number;
  };
  wife: {
    annualIncome: number;
    incomeGrowth: number;
    workStart: number;
    workEnd: number;
    retirementAge: number;
    retirementBonus: number;
    pension: number;
  };
  otherIncome: number;
}

export interface SavingsData {
  currentSavings: number;
  investmentAmount: number;
  monthlySavings: number;
  savingsGrowth: number;
  investmentReturn: number;
  inflationRate: number;
}

export interface ExpensesData {
  monthlyLivingCost: number;
  expenseGrowth: number;
  housingCost: number;
  housingEndAge: number | null;
  educationCostPerChild: number;
  carCost: number;
  insuranceCost: number;
}

export interface LifeEvent {
  id: string;
  name: string;
  age: string;
  cost: string;
}

export interface ChartDataPoint {
  year: number;
  age: number;
  assets: number;
  income: number;
  expenses: number;
}
