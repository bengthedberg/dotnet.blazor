# Notes on Steps

- **Create Solution**
    - `> mkdir \sandbox.aws.application`
    - `> cd \sandbox.aws.application`
    - `> git init`
    - `> echo '# sandbox.aws.application' > README.md`
    - `> mkdir provision,src,test,doc,scripts`
    - `> dotnet new gitignore`
    - `> dotnet new sln`     
    Commit
    - `> git add .`
    - `> git commit -m 'Create Solution'`     
    
 - **Add Infrastructure as Code**
    - `> cd provision`
    - `> cdk init app --language csharp`     
    - `> cd \sandbox.aws.application`
    - `> dotnet sln add .\provision\src\Provision\Provision.csproj`
        Commit
    - `> git add .`
    - `> git commit -m 'Add Infrastructure as Code'`   
        [Info on AWS CDK](https://docs.aws.amazon.com/cdk/index.html) 

 - **Add Cognito User Manager**
    - `> cd src`
    - `> dotnet new classlib -n UserManager.Contracts`
    - 