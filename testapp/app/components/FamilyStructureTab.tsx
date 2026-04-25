import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { FamilyData } from "../types";

interface FamilyStructureTabProps {
  data: FamilyData;
  onChange: (data: FamilyData) => void;
}

export function FamilyStructureTab({ data, onChange }: FamilyStructureTabProps) {
  const handleChange = (field: keyof FamilyData, value: string) => {
    const numValue = value === '' ? 0 : parseInt(value);
    onChange({ ...data, [field]: isNaN(numValue) ? null : numValue });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>家族構成</CardTitle>
        <CardDescription>ご家族の年齢を入力してください（予定は「-（マイナス）」で入力）</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* 本人・配偶者 */}
        <div className="space-y-4 p-4 bg-secondary/30 rounded-lg">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="person-age">本人の年齢</Label>
              <Input 
                id="person-age" 
                type="number" 
                placeholder="30"
                min="-5"
                max="100"
                value={data.personAge || ''}
                onChange={(e) => handleChange('personAge', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="spouse-age">配偶者の年齢</Label>
              <Input 
                id="spouse-age" 
                type="number" 
                placeholder="28"
                min="-5"
                max="100"
                value={data.spouseAge || ''}
                onChange={(e) => handleChange('spouseAge', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* 子ども */}
        <div className="space-y-4 p-4 bg-accent/30 rounded-lg">
          <h3 className="text-lg">お子さま</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="child1-age">第1子の年齢</Label>
              <Input 
                id="child1-age" 
                type="number" 
                placeholder="-2（2年後予定）"
                min="-20"
                max="50"
                value={data.child1Age ?? ''}
                onChange={(e) => handleChange('child1Age', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="child2-age">第2子の年齢</Label>
              <Input 
                id="child2-age" 
                type="number" 
                placeholder="-5（5年後予定）"
                min="-20"
                max="50"
                value={data.child2Age ?? ''}
                onChange={(e) => handleChange('child2Age', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="child3-age">第3子の年齢</Label>
              <Input 
                id="child3-age" 
                type="number" 
                placeholder="空欄可"
                min="-20"
                max="50"
                value={data.child3Age ?? ''}
                onChange={(e) => handleChange('child3Age', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="child4-age">第4子の年齢</Label>
              <Input 
                id="child4-age" 
                type="number" 
                placeholder="空欄可"
                min="-20"
                max="50"
                value={data.child4Age ?? ''}
                onChange={(e) => handleChange('child4Age', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="child5-age">第5子の年齢</Label>
              <Input 
                id="child5-age" 
                type="number" 
                placeholder="空欄可"
                min="-20"
                max="50"
                value={data.child5Age ?? ''}
                onChange={(e) => handleChange('child5Age', e.target.value)}
              />
            </div>
          </div>
        </div>

        <div className="p-3 bg-muted rounded-lg">
          <p className="text-sm text-muted-foreground">
            💡 ヒント：まだ生まれていないお子さまは、マイナスの数値で入力してください。
            例：2年後に出産予定の場合は「-2」と入力
          </p>
        </div>
      </CardContent>
    </Card>
  );
}