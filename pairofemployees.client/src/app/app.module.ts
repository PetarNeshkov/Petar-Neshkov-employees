import { NgModule } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EmployeeUploadComponent } from './employees/components/employee-upload/employee-upload.component';
import { EmployeeResultsComponent } from './employees/components/employee-results/employee-results.component';

@NgModule({
  declarations: [
    AppComponent, EmployeeUploadComponent, EmployeeResultsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [provideHttpClient()],
  bootstrap: [AppComponent]
})
export class AppModule { }
