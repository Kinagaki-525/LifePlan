import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { IncomeData } from "../types";

interface IncomeTabProps {
  data: IncomeData;
  onChange: (data: IncomeData) => void;
}

export function IncomeTab({ data, onChange }: IncomeTabProps) {
  const handleHusbandChange = (field: keyof IncomeData['husband'], value: string) => {
    const numValue = parseFloat(value) || 0;
    onChange({
      ...data,
      husband: { ...data.husband, [field]: numValue }
    });
  };

  const handleWifeChange = (field: keyof IncomeData['wife'], value: string) => {
    const numValue = parseFloat(value) || 0;
    onChange({
      ...data,
      wife: { ...data.wife, [field]: numValue }
    });
  };

  const handleOtherIncomeChange = (value: string) => {
    onChange({ ...data, otherIncome: parseFloat(value) || 0 });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>収入</CardTitle>
        <CardDescription>ご家族の収入に関する情報を入力してください</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* 本人収入 */}
        <div className="space-y-4 p-4 bg-secondary/30 rounded-lg">
          <h3 className="text-lg">本人（夫）の収入</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="husband-annual-income">現在の年収（万円）</Label>
              <Input 
                id="husband-annual-income" 
                type="number" 
                placeholder="500"
                min="0"
                step="10"
                value={data.husband.annualIncome || ''}
                onChange={(e) => handleHusbandChange('annualIncome', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="husband-income-growth">年収上昇率（%）</Label>
              <Input 
                id="husband-income-growth" 
                type="number" 
                placeholder="2"
                min="0"
                max="10"
                step="0.1"
                value={data.husband.incomeGrowth || ''}
                onChange={(e) => handleHusbandChange('incomeGrowth', e.target.value)}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="husband-work-period">就労期間（年齢）</Label>
              <div className="flex items-center gap-2">
                <Input 
                  id="husband-work-start" 
                  type="number" 
                  placeholder="22"
                  min="15"
                  max="100"
                  value={data.husband.workStart || ''}
                  onChange={(e) => handleHusbandChange('workStart', e.target.value)}
                />
                <span className="text-muted-foreground">〜</span>
                <Input 
                  id="husband-work-end" 
                  type="number" 
                  placeholder="65"
                  min="15"
                  max="100"
                  value={data.husband.workEnd || ''}
                  onChange={(e) => handleHusbandChange('workEnd', e.target.value)}
                />
                <span className="text-sm text-muted-foreground">歳</span>
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="husband-retirement-age">退職予定年齢</Label>
              <Input 
                id="husband-retirement-age" 
                type="number" 
                placeholder="65"
                min="50"
                max="80"
                value={data.husband.retirementAge || ''}
                onChange={(e) => handleHusbandChange('retirementAge', e.target.value)}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="husband-retirement-bonus">退職金予定額（万円）</Label>
              <Input 
                id="husband-retirement-bonus" 
                type="number" 
                placeholder="2000"
                min="0"
                step="100"
                value={data.husband.retirementBonus || ''}
                onChange={(e) => handleHusbandChange('retirementBonus', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="husband-pension">退職後の年金額（月額・万円）</Label>
              <Input 
                id="husband-pension" 
                type="number" 
                placeholder="15"
                min="0"
                step="0.1"
                value={data.husband.pension || ''}
                onChange={(e) => handleHusbandChange('pension', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* 配偶者収入 */}
        <div className="space-y-4 p-4 bg-accent/30 rounded-lg">
          <h3 className="text-lg">配偶者（妻）の収入</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="wife-annual-income">現在の年収（万円）</Label>
              <Input 
                id="wife-annual-income" 
                type="number" 
                placeholder="300"
                min="0"
                step="10"
                value={data.wife.annualIncome || ''}
                onChange={(e) => handleWifeChange('annualIncome', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="wife-income-growth">年収上昇率（%）</Label>
              <Input 
                id="wife-income-growth" 
                type="number" 
                placeholder="1"
                min="0"
                max="10"
                step="0.1"
                value={data.wife.incomeGrowth || ''}
                onChange={(e) => handleWifeChange('incomeGrowth', e.target.value)}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="wife-work-period">就労期間（年齢）</Label>
              <div className="flex items-center gap-2">
                <Input 
                  id="wife-work-start" 
                  type="number" 
                  placeholder="22"
                  min="15"
                  max="100"
                  value={data.wife.workStart || ''}
                  onChange={(e) => handleWifeChange('workStart', e.target.value)}
                />
                <span className="text-muted-foreground">〜</span>
                <Input 
                  id="wife-work-end" 
                  type="number" 
                  placeholder="65"
                  min="15"
                  max="100"
                  value={data.wife.workEnd || ''}
                  onChange={(e) => handleWifeChange('workEnd', e.target.value)}
                />
                <span className="text-sm text-muted-foreground">歳</span>
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="wife-retirement-age">退職予定年齢</Label>
              <Input 
                id="wife-retirement-age" 
                type="number" 
                placeholder="65"
                min="50"
                max="80"
                value={data.wife.retirementAge || ''}
                onChange={(e) => handleWifeChange('retirementAge', e.target.value)}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="wife-retirement-bonus">退職金予定額（万円）</Label>
              <Input 
                id="wife-retirement-bonus" 
                type="number" 
                placeholder="1000"
                min="0"
                step="100"
                value={data.wife.retirementBonus || ''}
                onChange={(e) => handleWifeChange('retirementBonus', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="wife-pension">退職後の年金額（月額・万円）</Label>
              <Input 
                id="wife-pension" 
                type="number" 
                placeholder="10"
                min="0"
                step="0.1"
                value={data.wife.pension || ''}
                onChange={(e) => handleWifeChange('pension', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* その他の収入 */}
        <div className="space-y-4 p-4 bg-muted rounded-lg">
          <h3 className="text-lg">その他の収入</h3>
          <div className="space-y-2">
            <Label htmlFor="other-income">その他の収入（年額・万円）</Label>
            <Input 
              id="other-income" 
              type="number" 
              placeholder="0"
              min="0"
              step="10"
              value={data.otherIncome || ''}
              onChange={(e) => handleOtherIncomeChange(e.target.value)}
            />
            <p className="text-sm text-muted-foreground">副業、不動産収入など</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
