import type { Entity } from "../types/api";
import { money } from "../services/api";
import { Empty } from "./Feedback";

export type Column = { key: string; label: string; money?: boolean };
function cell(value: unknown): string {
  if (value === null || value === undefined || value === "") return "—";
  if (typeof value === "boolean") return value ? "Sí" : "No";
  if (typeof value === "object") return "Ver detalle";
  return String(value);
}
export function DataTable({
  items,
  columns,
  currency,
}: {
  items: Entity[];
  columns: Column[];
  currency?: string;
}) {
  if (!items.length) return <Empty />;
  return (
    <div className="table-scroll">
      <table>
        <thead>
          <tr>
            {columns.map((c) => (
              <th key={c.key} className={c.money ? "numeric" : ""}>
                {c.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {items.map((item, index) => (
            <tr key={item.id || index}>
              {columns.map((c) => (
                <td key={c.key} className={c.money ? "numeric" : ""}>
                  {c.money ? money(item[c.key], currency) : cell(item[c.key])}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
