import { Component, EventEmitter, Output } from '@angular/core';
@Component({
  selector: 'app-searchbar',
  templateUrl: './searchbar.component.html'
})
export class SearchbarComponent {
  term = '';
  @Output() search = new EventEmitter<string>();
  doSearch() { this.search.emit(this.term?.trim() || ''); }
}
