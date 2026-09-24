export interface ProjectOverlap {
  employeeId1: number;
  employeeId2: number;
  projectId: number;
  daysWorked: number;
}

export interface EmployeePairResult {
  employeeId1: number;
  employeeId2: number;
  totalDays: number;
  projects: ProjectOverlap[];
}

export interface AnalysisResponse {
  pair: EmployeePairResult | null;
}
