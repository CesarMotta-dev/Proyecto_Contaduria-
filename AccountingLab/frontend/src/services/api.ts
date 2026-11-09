import type { Problem } from "../types/api";

export class ApiError extends Error {
  constructor(
    public status: number,
    public problem: Problem,
  ) {
    super(
      problem.detail ||
        problem.title ||
        `La solicitud devolvió HTTP ${status}.`,
    );
  }
}

export async function request<T>(
  path: string,
  token?: string,
  companyId?: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");
  if (options.body) headers.set("Content-Type", "application/json");
  if (token) headers.set("Authorization", `Bearer ${token}`);
  if (companyId) headers.set("X-Company-Id", companyId);
  let response: Response;
  try {
    response = await fetch(`${import.meta.env.VITE_API_URL || "/api"}${path}`, {
      ...options,
      headers,
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError")
      throw error;
    throw new ApiError(0, {
      title: "Sin conexión",
      detail:
        "No se pudo conectar con la API. Revisa el servicio y vuelve a intentarlo.",
    });
  }
  const body = await response.json().catch(() => ({}));
  if (!response.ok) throw new ApiError(response.status, body);
  return body as T;
}

// Formatting only: monetary values remain decimal strings throughout the client.
export function money(value: unknown, currency = ""): string {
  if (typeof value !== "string" || !/^-?\d+(\.\d+)?$/.test(value)) return "—";
  const negative = value.startsWith("-");
  const [whole, fraction = ""] = (negative ? value.slice(1) : value).split(".");
  const grouped = whole.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
  const decimals = fraction.padEnd(2, "0");
  return `${negative ? "−" : ""}${grouped},${decimals}${currency ? ` ${currency}` : ""}`;
}

export const isDecimal = (value: string, precision = 4) =>
  new RegExp(`^\\d{1,15}(\\.\\d{1,${precision}})?$`).test(value);
