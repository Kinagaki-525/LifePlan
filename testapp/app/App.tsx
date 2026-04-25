import { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./components/ui/tabs";
import { Button } from "./components/ui/button";
import { FamilyStructureTab } from "./components/FamilyStructureTab";
import { IncomeTab } from "./components/IncomeTab";
import { SavingsTab } from "./components/SavingsTab";
import { ExpensesTab } from "./components/ExpensesTab";
import { LifeEventsTab } from "./components/LifeEventsTab";
import { FinancialChart } from "./components/FinancialChart";
import { FamilyData, IncomeData, SavingsData, ExpensesData, LifeEvent, ChartDataPoint } from "./types";
import { calculateFinancialPlan } from "./utils/calculator";

export default function App() {
  const [showChart, setShowChart] = useState(false);
  const [chartData, setChartData] = useState<ChartDataPoint[]>([]);

  // 家族構成データ
  const [familyData, setFamilyData] = useState<FamilyData>({
    personAge: 30,
    spouseAge: 28,
    child1Age: null,
    child2Age: null,
    child3Age: null,
    child4Age: null,
    child5Age: null,
  });

  // 収入データ
  const [incomeData, setIncomeData] = useState<IncomeData>({
    husband: {
      annualIncome: 500,
      incomeGrowth: 2,
      workStart: 22,
      workEnd: 65,
      retirementAge: 65,
      retirementBonus: 2000,
      pension: 15,
    },
    wife: {
      annualIncome: 300,
      incomeGrowth: 1,
      workStart: 22,
      workEnd: 65,
      retirementAge: 65,
      retirementBonus: 1000,
      pension: 10,
    },
    otherIncome: 0,
  });

  // 貯蓄データ
  const [savingsData, setSavingsData] = useState<SavingsData>({
    currentSavings: 300,
    investmentAmount: 100,
    monthlySavings: 10,
    savingsGrowth: 0,
    investmentReturn: 3,
    inflationRate: 1,
  });

  // 支出データ
  const [expensesData, setExpensesData] = useState<ExpensesData>({
    monthlyLivingCost: 25,
    expenseGrowth: 1,
    housingCost: 10,
    housingEndAge: null,
    educationCostPerChild: 1000,
    carCost: 30,
    insuranceCost: 20,
  });

  // ライフイベントデータ
  const [lifeEvents, setLifeEvents] = useState<LifeEvent[]>([
    { id: '1', name: '', age: '', cost: '' }
  ]);

  const handleShowResults = () => {
    // 計算実行
    const calculatedData = calculateFinancialPlan(
      familyData,
      incomeData,
      savingsData,
      expensesData,
      lifeEvents
    );
    
    setChartData(calculatedData);
    setShowChart(true);
  };

  return (
    <div className="min-h-screen bg-background p-4 md:p-8">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* ヘッダー */}
        <div className="text-center space-y-3 py-6">
          <div className="inline-block px-8 py-4 bg-gradient-to-r from-primary/10 via-secondary/20 to-accent/10 rounded-2xl">
            <h1 className="text-4xl md:text-5xl tracking-tight bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              夫婦の羅針盤
            </h1>
            <p className="text-xl md:text-2xl text-muted-foreground mt-2">ライフプランシュミレーター</p>
          </div>
          <p className="text-sm text-muted-foreground max-w-2xl mx-auto">
            お二人の未来を一緒に描きましょう。各タブで情報を入力して、将来の資産推移をシミュレーションできます。
          </p>
        </div>

        {/* タブエリア */}
        <Tabs defaultValue="family" className="w-full">
          <TabsList className="grid w-full grid-cols-5 h-auto gap-1 bg-secondary/30 p-1">
            <TabsTrigger value="family" className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              家族構成
            </TabsTrigger>
            <TabsTrigger value="income" className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              収入
            </TabsTrigger>
            <TabsTrigger value="savings" className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              貯蓄
            </TabsTrigger>
            <TabsTrigger value="expenses" className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              支出
            </TabsTrigger>
            <TabsTrigger value="events" className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
              ライフイベント
            </TabsTrigger>
          </TabsList>

          <TabsContent value="family" className="mt-6">
            <FamilyStructureTab data={familyData} onChange={setFamilyData} />
          </TabsContent>

          <TabsContent value="income" className="mt-6">
            <IncomeTab data={incomeData} onChange={setIncomeData} />
          </TabsContent>

          <TabsContent value="savings" className="mt-6">
            <SavingsTab data={savingsData} onChange={setSavingsData} />
          </TabsContent>

          <TabsContent value="expenses" className="mt-6">
            <ExpensesTab data={expensesData} onChange={setExpensesData} />
          </TabsContent>

          <TabsContent value="events" className="mt-6">
            <LifeEventsTab data={lifeEvents} onChange={setLifeEvents} />
          </TabsContent>
        </Tabs>

        {/* 結果を表示ボタン */}
        <div className="flex justify-center py-4">
          <Button 
            onClick={handleShowResults}
            size="lg"
            className="px-16 py-6 text-lg shadow-lg hover:shadow-xl transition-all duration-300 bg-primary hover:bg-primary/90"
          >
            ✨ 結果を表示
          </Button>
        </div>

        {/* グラフエリア */}
        {showChart && chartData.length > 0 && (
          <div className="animate-in fade-in duration-500">
            <FinancialChart data={chartData} />
          </div>
        )}
      </div>
    </div>
  );
}
