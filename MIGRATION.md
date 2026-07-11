# Migration from the 2012 version

The safest migration keeps the old repository history intact and develops the remaster on a separate branch.

## 1. Clone the repository

```powershell
git clone https://github.com/Barath0n/blackjack-1.git
cd blackjack-1
```

## 2. Refresh the current legacy state

```powershell
git switch master
git pull --ff-only origin master
```

## 3. Preserve v0.1.9pre as a branch

```powershell
git branch legacy-v0.1.9pre
git push -u origin legacy-v0.1.9pre
```

This creates a browsable, maintainable legacy branch at exactly the current `master` commit.

## 4. Preserve the same commit as an annotated tag

```powershell
git tag -a v0.1.9pre -m "Legacy Blackjack v0.1.9pre (2012)"
git push origin v0.1.9pre
```

The tag is the immutable historical snapshot. The branch is useful only if an old-version fix is ever needed.

## 5. Create the remaster branch

```powershell
git switch master
git switch -c remaster
git push -u origin remaster
```

## 6. Replace the working tree with the remaster starter

Delete the old application files on `remaster`, but keep `.git` and `LICENSE`.

A safe approach in PowerShell is:

```powershell
git rm -r "Black Jack"
git rm -f "Blackjack.sln" "Blackjack.suo" "changes.txt" "README.md"
```

Some paths may not exist in every checkout; that is harmless. Then copy the contents of the remaster starter ZIP into the repository root.

Review the result before committing:

```powershell
git status
git diff --stat
```

Commit and push:

```powershell
git add .
git commit -m "Start Blackjack Remastered"
git push
```

## 7. Make the remaster the default later

Do this only after the app builds and the first milestone is usable.

On GitHub:

1. Open **Settings**
2. Open **Branches**
3. Change the default branch from `master` to `remaster`

Later, rename `remaster` to `main`:

```powershell
git branch -m remaster main
git push -u origin main
git push origin --delete remaster
```

Then change the GitHub default branch to `main`.

## Why branch and tag?

- `legacy-v0.1.9pre` branch: easy to browse and patch
- `v0.1.9pre` tag: immutable historical release marker
- `remaster` branch: safe workspace for the rewrite
- no duplicated legacy source folders in the modern application
