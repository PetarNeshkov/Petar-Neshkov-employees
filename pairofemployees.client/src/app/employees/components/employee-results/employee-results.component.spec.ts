import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmployeeResultsComponent } from './employee-results.component';

describe('EmployeeResultsComponent', () => {
  let fixture: ComponentFixture<EmployeeResultsComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule], declarations: [EmployeeResultsComponent]
    }).compileComponents();
    fixture = TestBed.createComponent(EmployeeResultsComponent);
  });

  it('shows an empty state when no pair overlaps', () => {
    fixture.componentRef.setInput('pair', null);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No shared working time found');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });

  it('renders the pair total and every project row', () => {
    fixture.componentRef.setInput('pair', {
      employeeId1: 12, employeeId2: 34, totalDays: 1002,
      projects: [
        { employeeId1: 12, employeeId2: 34, projectId: 10, daysWorked: 1000 },
        { employeeId1: 12, employeeId2: 34, projectId: 20, daysWorked: 2 }
      ]
    });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.summary').textContent).toContain('1,002');
    const rows: HTMLElement[] = Array.from(fixture.nativeElement.querySelectorAll('tbody tr'));
    expect(rows.map(row => Array.from(row.querySelectorAll('td')).map(cell => cell.textContent?.trim())))
      .toEqual([['12', '34', '10', '1,000'], ['12', '34', '20', '2']]);
    expect(fixture.nativeElement.textContent).not.toContain('No shared working time found');
  });
});
