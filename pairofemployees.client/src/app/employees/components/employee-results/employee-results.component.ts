import { Component, Input } from '@angular/core';
import { EmployeePairResult } from '../../models/analysis-response';

@Component({
  selector: 'app-employee-results',
  templateUrl: './employee-results.component.html',
  styleUrl: './employee-results.component.css'
})
export class EmployeeResultsComponent {
  @Input({ required: true }) pair: EmployeePairResult | null = null;
}
