import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { ChartDataPoint } from '../types';

interface FinancialChartProps {
  data: ChartDataPoint[];
}

export function FinancialChart({ data }: FinancialChartProps) {
  return (
    <Card className="shadow-lg border-2 border-primary/20">
      <CardHeader className="bg-gradient-to-r from-secondary/40 to-accent/40">
        <CardTitle className="text-2xl text-center">💰 資産推移シミュレーション</CardTitle>
      </CardHeader>
      <CardContent className="pt-6">
        <ResponsiveContainer width="100%" height={450}>
          <LineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f0d9d4" />
            <XAxis 
              dataKey="year" 
              label={{ value: '年', position: 'insideBottomRight', offset: -5 }}
              stroke="#8d8d8d"
            />
            <YAxis 
              label={{ value: '金額（円）', angle: -90, position: 'insideLeft' }}
              tickFormatter={(value) => `${(value / 10000).toLocaleString()}万`}
              stroke="#8d8d8d"
            />
            <Tooltip 
              formatter={(value: number) => `${value.toLocaleString()}円`}
              labelFormatter={(label) => `${label}年`}
              contentStyle={{ 
                backgroundColor: '#ffffff', 
                border: '2px solid #f0d9d4',
                borderRadius: '8px'
              }}
            />
            <Legend 
              wrapperStyle={{ paddingTop: '20px' }}
            />
            <Line 
              type="monotone" 
              dataKey="assets" 
              stroke="#e88b8f" 
              name="総資産"
              strokeWidth={3}
              dot={{ fill: '#e88b8f', r: 4 }}
              activeDot={{ r: 6 }}
            />
            <Line 
              type="monotone" 
              dataKey="income" 
              stroke="#b8d8e0" 
              name="年間収入"
              strokeWidth={2}
              dot={{ fill: '#b8d8e0', r: 3 }}
            />
            <Line 
              type="monotone" 
              dataKey="expenses" 
              stroke="#f4d6cc" 
              name="年間支出"
              strokeWidth={2}
              dot={{ fill: '#f4d6cc', r: 3 }}
            />
          </LineChart>
        </ResponsiveContainer>
        <div className="mt-6 p-4 bg-gradient-to-r from-secondary/30 to-accent/30 rounded-lg">
          <p className="text-sm text-center text-muted-foreground">
            入力データに基づいて計算された{data.length}年間の資産推移を表示しています
          </p>
        </div>
      </CardContent>
    </Card>
  );
}