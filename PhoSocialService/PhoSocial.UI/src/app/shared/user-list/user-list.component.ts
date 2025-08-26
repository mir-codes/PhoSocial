import { Component, Input, Output, EventEmitter } from '@angular/core';
@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent {
  @Input() users: any[] = [];
  @Input() selectedUserId: string = '';
  @Output() selectUser = new EventEmitter<string>();

  onSelect(id: string) {
    this.selectUser.emit(id);
  }
}
