# Git Commit Standards

When generating git commit messages or making commits:

1. **Format:** Follow the Conventional Commits specification:
   `<type>(<scope>): <short imperative description>`

   [optional body explaining *why* the change was made]

2. **Allowed Types:**
   - `feat`: Net-new user/system capability or endpoint
   - `fix`: Bug fix, error resolution, or security patch
   - `refactor`: Code reorganization without behavior change
   - `perf`: Performance improvement
   - `docs`: Documentation updates or comments
   - `chore`: Tooling, dependencies, or configuration changes
   - `test`: Adding or modifying automated tests

3. **Scopes:** Always specify the affected area, for example:
   - `backend`, `frontend`, `auth`, `rbac`, `db`, `session`, `faculty`

4. **Rules for Summary Line:**
   - Max 72 characters.
   - Use the imperative mood ("add", "implement", "fix", not "added" or "fixing").
   - Do NOT use vague terms like "update files", "clean up", "misc fixes", or "WIP".

5. **Body Format (for multi-file changes):**
   - Provide concise bullet points detailing key changes and their architectural intent.