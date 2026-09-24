export interface ApiProblem {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
