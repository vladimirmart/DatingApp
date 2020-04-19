import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private routerService: Router, private notifyts: AlertifyService) { }
  canActivate(): boolean {
    if (this.authService.loggedIn()) {
      return true;
    }
    this.routerService.navigate(['/home']);
    this.notifyts.error('Lo siento papipo');
    return false;
  }

}
