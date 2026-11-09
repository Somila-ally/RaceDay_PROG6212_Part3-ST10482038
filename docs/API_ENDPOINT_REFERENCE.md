# RaceDay API endpoint reference

All routes use the `/api` prefix. Session-protected routes require a successful login. Organiser-owned routes also verify ownership of the event.

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/auth/register` | Public | Create an Organiser or Participant account; password is hashed. |
| POST | `/auth/login` | Public | Start the server-side session. |
| POST | `/auth/logout` | Signed in | Clear the current session. |
| GET | `/profile/me` | Signed in | Read the current user's profile. |
| PUT | `/profile/me` | Signed in | Update the current user's name. |
| GET | `/events` | Signed in | Browse events, ordered by date. |
| GET | `/events/{id}` | Signed in | Read event details. |
| GET | `/events/mine` | Organiser | List events owned by the current organiser. |
| POST | `/events` | Organiser | Create an event. |
| PUT | `/events/{id}` | Owning organiser | Update an event. |
| DELETE | `/events/{id}` | Owning organiser | Delete an event if it has no enrolments. |
| GET | `/events/{eventId}/categories` | Signed in | List an event's categories. |
| POST | `/events/{eventId}/categories` | Owning organiser | Add a category. |
| PUT | `/categories/{id}` | Owning organiser | Update a category. |
| DELETE | `/categories/{id}` | Owning organiser | Delete a category if it has no enrolments. |
| POST | `/enrolments` | Participant | Enter an event category. |
| GET | `/enrolments/mine` | Participant | List the current participant's entries. |
| GET | `/enrolments/event/{eventId}` | Owning organiser | List entries for an event. |
| PUT | `/enrolments/{id}/status` | Owning organiser | Change an entry's status. |
| POST | `/results` | Owning organiser | Record an enrolled participant's finish time and position after the event. |
| GET | `/results/mine` | Participant | View the participant's own results. |
| GET | `/results/event/{eventId}` | Owning organiser | List results for an event, ordered by place. |
| POST | `/images/events/{eventId}/banner` | Owning organiser | Upload an event banner through Azure Blob Storage. |
| POST | `/images/profile` | Signed in | Upload a profile image through Azure Blob Storage. |

The API is served at `/api`; for example, the login route is `POST /api/auth/login`. Swagger at `/swagger` lists the request payloads and response status codes.

## Result rules

- The enrolment must exist and belong to an event owned by the signed-in Organiser.
- The enrolment cannot be cancelled.
- Finish time must parse as a positive duration.
- The event date/time must have passed.
- An enrolment can have only one result, and a finishing position can be used only once within the same event category.

## Image upload rules

Only JPEG, PNG and WebP files up to 5 MB are accepted. Azure configuration is supplied to the API process with `AzureBlobStorage__ConnectionString` and `AzureBlobStorage__ContainerName`. The container uses public read access for image blobs so the MVC site can display the returned URLs; do not upload sensitive files.
