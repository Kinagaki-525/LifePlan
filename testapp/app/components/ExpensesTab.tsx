import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { ExpensesData } from "../types";

interface ExpensesTabProps {
  data: ExpensesData;
  onChange: (data: ExpensesData) => void;
}

export function ExpensesTab({ data, onChange }: ExpensesTabProps) {
  const handleChange = (field: keyof ExpensesData, value: string) => {
    if (field === 'housingEndAge') {
      const numValue = value === '' ? null : parseInt(value);
      onChange({ ...data, [field]: numValue });
    } else {
      const numValue = parseFloat(value) || 0;
      onChange({ ...data, [field]: numValue });
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>支出</CardTitle>
        <CardDescription>毎月・毎年の支出に関する情報を入力してください</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* 基本生活費 */}
        <div className="space-y-4 p-4 bg-secondary/30 rounded-lg">
          <h3 className="text-lg">基本生活費</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="monthly-living-cost">月々の生活費（万円）</Label>
              <Input 
                id="monthly-living-cost" 
                type="number" 
                placeholder="25"
                min="0"
                step="1"
                value={data.monthlyLivingCost || ''}
                onChange={(e) => handleChange('monthlyLivingCost', e.target.value)}
              />
              <p className="text-sm text-muted-foreground">食費、光熱費、通信費など</p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="expense-growth">支出上昇率（%/年）</Label>
              <Input 
                id="expense-growth" 
                type="number" 
                placeholder="1"
                min="0"
                max="10"
                step="0.1"
                value={data.expenseGrowth || ''}
                onChange={(e) => handleChange('expenseGrowth', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* 住居費 */}
        <div className="space-y-4 p-4 bg-accent/30 rounded-lg">
          <h3 className="text-lg">住居費</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="housing-cost">家賃・住宅ローン（月額・万円）</Label>
              <Input 
                id="housing-cost" 
                type="number" 
                placeholder="10"
                min="0"
                step="1"
                value={data.housingCost || ''}
                onChange={(e) => handleChange('housingCost', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="housing-end-age">支払い終了年齢</Label>
              <Input 
                id="housing-end-age" 
                type="number" 
                placeholder="60"
                min="0"
                max="100"
                value={data.housingEndAge ?? ''}
                onChange={(e) => handleChange('housingEndAge', e.target.value)}
              />
              <p className="text-sm text-muted-foreground">空欄の場合は継続</p>
            </div>
          </div>
        </div>

        {/* 教育費 */}
        <div className="space-y-4 p-4 bg-muted rounded-lg">
          <h3 className="text-lg">教育費</h3>
          <div className="space-y-2">
            <Label htmlFor="education-cost-per-child">子ども1人あたりの総教育費（万円）</Label>
            <Input 
              id="education-cost-per-child" 
              type="number" 
              placeholder="1000"
              min="0"
              step="50"
              value={data.educationCostPerChild || ''}
              onChange={(e) => handleChange('educationCostPerChild', e.target.value)}
            />
            <p className="text-sm text-muted-foreground">幼稚園〜大学卒業までの総額目安：公立中心800万円、私立中心2000万円</p>
          </div>
        </div>

        {/* その他の支出 */}
        <div className="space-y-4 p-4 bg-secondary/20 rounded-lg">
          <h3 className="text-lg">その他の年間支出</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="car-cost">車関連費用（年額・万円）</Label>
              <Input 
                id="car-cost" 
                type="number" 
                placeholder="30"
                min="0"
                step="5"
                value={data.carCost || ''}
                onChange={(e) => handleChange('carCost', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="insurance-cost">保険料（年額・万円）</Label>
              <Input 
                id="insurance-cost" 
                type="number" 
                placeholder="20"
                min="0"
                step="5"
                value={data.insuranceCost || ''}
                onChange={(e) => handleChange('insuranceCost', e.target.value)}
              />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
