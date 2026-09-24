import { environment } from '../../../environments/environment';

const baseApiUrl = `${environment.baseUrl}/api`;
const employeePairsControllerRoute = `${baseApiUrl}/EmployeePairs`;

export const EMPLOYEE_PAIR_URLS = {
  ANALYZE: `${employeePairsControllerRoute}/Analyze`
};
