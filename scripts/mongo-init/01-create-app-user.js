const dbName = process.env.MONGO_INITDB_DATABASE || "projecttemplate";
const appUser = process.env.MONGO_APP_USERNAME || "admin";
const appPassword = process.env.MONGO_APP_PASSWORD || "admin";

const appDb = db.getSiblingDB(dbName);

const existingUser = appDb.getUser(appUser);
if (!existingUser) {
  appDb.createUser({
    user: appUser,
    pwd: appPassword,
    roles: [{ role: "readWrite", db: dbName }]
  });
  print(`Created MongoDB app user '${appUser}' for database '${dbName}'.`);
} else {
  print(`MongoDB app user '${appUser}' already exists for database '${dbName}'.`);
}
