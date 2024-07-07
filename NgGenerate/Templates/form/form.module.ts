import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { %NAME%RoutingModule } from './%name%-routing.module';
import { %NAME%Component } from '../%name%/%name%.component';
import { ComponentModule } from '../../components/component.module';


@NgModule({
  declarations: [
    %NAME%Component
  ],
  imports: [
    CommonModule,
    %NAME%RoutingModule,
    ComponentModule
  ]
})
export class %NAME%Module { }
