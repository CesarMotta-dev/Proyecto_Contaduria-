export type Entity = { id: string; [key: string]: unknown };
export type Page<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};
export type Company = {
  id: string;
  name: string;
  currency: string;
  role: string;
};
export type Session = {
  accessToken: string;
  user: { id: string; name: string; email: string };
  companies: Company[];
};
export type Problem = {
  title?: string;
  detail?: string;
  status?: number;
  challengeId?: string;
  patternName?: string;
  referenceUrl?: string;
  errors?: Record<string, string[]>;
};
export type Challenge = {
  id: string;
  name: string;
  family: string;
  context: string;
  referenceUrl: string;
  available: boolean;
};
export type Summary = {
  debits: string;
  credits: string;
  balance: string;
  entryCount: number;
  receivableTotal: string;
  payableTotal: string;
};
