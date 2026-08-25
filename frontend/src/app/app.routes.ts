import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { InventoryListComponent } from './features/inventory/inventory-list.component';
import { SupplierListComponent } from './features/suppliers/supplier-list.component';
import { PosComponent } from './features/pos/pos.component';
import { PurchaseRequestsComponent } from './features/purchase-requests/purchase-requests.component';
import { ReportsComponent } from './features/reports/reports.component';
import { NotificationsComponent } from './features/notifications/notifications.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth/login',
    component: LoginComponent
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'pos', component: PosComponent },
      { path: 'purchase-requests', component: PurchaseRequestsComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'notifications', component: NotificationsComponent },
      { path: 'inventory', component: InventoryListComponent },
      { path: 'suppliers', component: SupplierListComponent }
    ]
  },

  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
