import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Plus, Trash2 } from "lucide-react";
import { LifeEvent } from "../types";

interface LifeEventsTabProps {
  data: LifeEvent[];
  onChange: (data: LifeEvent[]) => void;
}

export function LifeEventsTab({ data, onChange }: LifeEventsTabProps) {
  const addEvent = () => {
    onChange([...data, { 
      id: Date.now().toString(), 
      name: '', 
      age: '', 
      cost: '' 
    }]);
  };

  const removeEvent = (id: string) => {
    if (data.length > 1) {
      onChange(data.filter(event => event.id !== id));
    }
  };

  const updateEvent = (id: string, field: keyof LifeEvent, value: string) => {
    onChange(data.map(event => 
      event.id === id ? { ...event, [field]: value } : event
    ));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>ライフイベント</CardTitle>
        <CardDescription>将来予定している大きな支出を入力してください</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* イベント例 */}
        <div className="p-4 bg-muted rounded-lg">
          <p className="text-sm">
            <span className="font-semibold">入力例：</span>
            結婚式（150万円）、新婚旅行（50万円）、車購入（300万円）、
            住宅購入頭金（500万円）、子どもの留学（200万円）など
          </p>
        </div>

        {/* イベント入力フォーム */}
        <div className="space-y-4">
          {data.map((event, index) => (
            <div key={event.id} className="p-4 bg-secondary/20 rounded-lg space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg">イベント {index + 1}</h3>
                {data.length > 1 && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => removeEvent(event.id)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                )}
              </div>
              
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2 col-span-1">
                  <Label htmlFor={`event-name-${event.id}`}>イベント名</Label>
                  <Input 
                    id={`event-name-${event.id}`}
                    placeholder="例：新車購入"
                    value={event.name}
                    onChange={(e) => updateEvent(event.id, 'name', e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor={`event-age-${event.id}`}>実施年齢</Label>
                  <Input 
                    id={`event-age-${event.id}`}
                    type="number"
                    placeholder="35"
                    value={event.age}
                    onChange={(e) => updateEvent(event.id, 'age', e.target.value)}
                    min="0"
                    max="100"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor={`event-cost-${event.id}`}>費用（万円）</Label>
                  <Input 
                    id={`event-cost-${event.id}`}
                    type="number"
                    placeholder="300"
                    value={event.cost}
                    onChange={(e) => updateEvent(event.id, 'cost', e.target.value)}
                    min="0"
                    step="10"
                  />
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* イベント追加ボタン */}
        <Button 
          onClick={addEvent}
          variant="outline"
          className="w-full"
        >
          <Plus className="h-4 w-4 mr-2" />
          イベントを追加
        </Button>
      </CardContent>
    </Card>
  );
}