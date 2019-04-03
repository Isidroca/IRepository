<h2>
     Other simple sql object mapper for .Net
</h2>

<h2>
     Install
  
</h2>
<code>
    Install-Package IEntityRepository
  </code>
  </ br>
  <p>or visit <a href="https://www.nuget.org/packages/IEntityRepository/">nuget</a> for other version</p>
<h2>
     Usage
</h2>
<div class="highlight highlight-source-cs">
  <pre>
    EntityDataAccess repo = new EntityDataAccess(YouConnectionString);
      
      repo.CommandType = EntityDataAccess.ICommandType.Text;
      repo.CommandText = @"select * from Person";
      
      var data = repo.FirstOrDefault<Person>();
  </pre>
 </div>
