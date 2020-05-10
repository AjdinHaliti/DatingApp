import {Routes} from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListstComponent } from './listst/listst.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolvers';
import { MemberListlResolver } from './_resolvers/member-list.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberEditResolver } from './_resolvers/member-edit.resolvers';
import { UnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';

export const appRoutes: Routes = [
    {path: '', component: HomeComponent},
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            {path: 'members', component: MemberListComponent, resolve: {users: MemberListlResolver}},
            {path: 'members/:id', component: MemberDetailComponent, resolve: {user: MemberDetailResolver}},
            {path: 'member/edit', component: MemberEditComponent,
                 resolve: {user: MemberEditResolver}, canDeactivate: [UnsavedChangesGuard]},
            {path: 'messages', component: MessagesComponent},
            {path: 'lists', component: ListstComponent},
        ]
    },
    {path: '**', redirectTo: '', pathMatch: 'full'},
];
