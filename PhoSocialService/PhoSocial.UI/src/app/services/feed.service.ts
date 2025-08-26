import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class FeedService {
  private base = environment.apiUrl + '/Feed';
  constructor(private http: HttpClient) {}

  getPosts() { return this.http.get<any[]>(`${this.base}/posts`); }
  getLikes(postId: string) { return this.http.get<number>(`${this.base}/likes/${postId}`); }
  getComments(postId: string) { return this.http.get<any[]>(`${this.base}/comments/${postId}`); }

  createPost(caption: string, image?: File) {
    const fd = new FormData();
    fd.append('Caption', caption || '');
    if (image) fd.append('Image', image, image.name);
    return this.http.post<any>(`${this.base}/posts`, fd);
  }

  like(postId: string) { return this.http.post(`${this.base}/like/${postId}`, {}); }
  unlike(postId: string) { return this.http.post(`${this.base}/unlike/${postId}`, {}); }
  comment(postId: string, content: string) { return this.http.post(`${this.base}/comment/${postId}`, content); }
}
