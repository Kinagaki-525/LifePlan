import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { SavingsData } from "../types";

interface SavingsTabProps {
  data: SavingsData;
  onChange: (data: SavingsData) => void;
}

export function SavingsTab({ data, onChange }: SavingsTabProps) {
  const handleChange = (field: keyof SavingsData, value: string) => {
    const numValue = parseFloat(value) || 0;
    onChange({ ...data, [field]: numValue });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>貯蓄</CardTitle>
        <CardDescription>現在の資産と運用に関する情報を入力してください</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* 現在の資産 */}
        <div className="space-y-4 p-4 bg-secondary/30 rounded-lg">
          <h3 className="text-lg">現在の資産</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="current-savings">現在の貯蓄額（万円）</Label>
              <Input 
                id="current-savings" 
                type="number" 
                placeholder="300"
                min="0"
                step="10"
                value={data.currentSavings || ''}
                onChange={(e) => handleChange('currentSavings', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="investment-amount">投資額（万円）</Label>
              <Input 
                id="investment-amount" 
                type="number" 
                placeholder="100"
                min="0"
                step="10"
                value={data.investmentAmount || ''}
                onChange={(e) => handleChange('investmentAmount', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* 毎月の貯蓄 */}
        <div className="space-y-4 p-4 bg-accent/30 rounded-lg">
          <h3 className="text-lg">毎月の貯蓄</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="monthly-savings">毎月の貯蓄額（万円）</Label>
              <Input 
                id="monthly-savings" 
                type="number" 
                placeholder="10"
                min="0"
                step="1"
                value={data.monthlySavings || ''}
                onChange={(e) => handleChange('monthlySavings', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="savings-growth">貯蓄額の上昇率（%/年）</Label>
              <Input 
                id="savings-growth" 
                type="number" 
                placeholder="0"
                min="0"
                max="10"
                step="0.1"
                value={data.savingsGrowth || ''}
                onChange={(e) => handleChange('savingsGrowth', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* 運用利回り */}
        <div className="space-y-4 p-4 bg-muted rounded-lg">
          <h3 className="text-lg">資産運用</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="investment-return">想定運用利回り（%/年）</Label>
              <Input 
                id="investment-return" 
                type="number" 
                placeholder="3"
                min="0"
                max="15"
                step="0.1"
                value={data.investmentReturn || ''}
                onChange={(e) => handleChange('investmentReturn', e.target.value)}
              />
              <p className="text-sm text-muted-foreground">一般的には2〜5%程度</p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="inflation-rate">インフレ率（%/年）</Label>
              <Input 
                id="inflation-rate" 
                type="number" 
                placeholder="1"
                min="0"
                max="10"
                step="0.1"
                value={data.inflationRate || ''}
                onChange={(e) => handleChange('inflationRate', e.target.value)}
              />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
