import { environment } from '../../../environments/environment';

const baseApiUrl = `${environment.baseUrl}/api`;
const employeePairsControllerRoute = `${baseApiUrl}/employee-pairs`;

export const EMPLOYEE_PAIR_URLS = {
  ANALYZE: `${employeePairsControllerRoute}/analyze`
};
