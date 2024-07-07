import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { %NAME%Component } from './%name%.component';

const routes: Routes = [
  {
    path: '',
    component: %NAME%Component,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class %NAME%RoutingModule { }
