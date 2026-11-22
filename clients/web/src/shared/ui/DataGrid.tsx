"use client";

import type { ReactNode } from "react";
import { useT } from "@/shared/i18n/I18nProvider";

export interface GridColumn<T> {
  readonly key: string;
  readonly header: ReactNode;
  readonly cell: (row: T) => ReactNode;
  /** Sayı ve tutar sütunları sağa hizalanır, eşit genişlikli rakam kullanır. */
  readonly align?: "start" | "end";
  readonly className?: string;
}

interface DataGridProps<T> {
  readonly columns: readonly GridColumn<T>[];
  /** undefined: veri henüz yok (yükleniyor ya da hata). */
  readonly rows: readonly T[] | undefined;
  readonly rowKey: (row: T) => string;
  readonly rowClassName?: (row: T) => string | undefined;
  readonly loading?: boolean;
  readonly density?: "standard" | "compact";
  readonly label?: string;
}

function columnClass<T>(column: GridColumn<T>): string | undefined {
  return [column.align === "end" ? "num" : null, column.className].filter(Boolean).join(" ") || undefined;
}

/** Yoğun veri gridi (Spec 013 KK5): sabit başlık, sayı hizalama, boş/yükleniyor durumu, durum satırları. */
export function DataGrid<T>({ columns, rows, rowKey, rowClassName, loading, density = "standard", label }: DataGridProps<T>) {
  const t = useT();
  const status = loading && !rows ? t("app.loading") : rows && rows.length === 0 ? t("common.noRecords") : null;
  return (
    <table className={density === "compact" ? "grid compact" : "grid"} aria-label={label}>
      <thead>
        <tr>
          {columns.map((column) => (
            <th key={column.key} scope="col" className={columnClass(column)}>
              {column.header}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {status ? (
          <tr>
            <td colSpan={columns.length} className="empty">
              {status}
            </td>
          </tr>
        ) : null}
        {rows?.map((row) => (
          <tr key={rowKey(row)} className={rowClassName?.(row)}>
            {columns.map((column) => (
              <td key={column.key} className={columnClass(column)}>
                {column.cell(row)}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
